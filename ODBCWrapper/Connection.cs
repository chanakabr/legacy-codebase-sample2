using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Configuration;
using System.Text.RegularExpressions;
using KLogMonitor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ODBCWrapper
{
    public class Connection
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static List<string> db_Slaves = (!string.IsNullOrEmpty(TCMClient.Settings.Instance.GetValue<string>("DB_Slaves_IPs"))) ? TCMClient.Settings.Instance.GetValue<string>("DB_Slaves_List").Split(':').ToList<string>() : null;
        public static bool m_bIsWritable;

        public Connection()
        {
        }

        private SqlConnection m_conn = null;

        static private string m_sConnectionStr = "";
        protected object m_crit_sec = new object();

        static public void ClearConnection()
        {
            m_sConnectionStr = "";
        }

        //TODO : add connection string for WRITABLE 
        static public string GetConnectionStringByKey(string sKey, bool bIsWritable)
        {
            string sRet = "";
            string applicationIntent = (bIsWritable) ? "ReadWrite" : "ReadOnly";
            m_bIsWritable = bIsWritable;


            if (Utils.GetTcmConfigValue(sKey) != string.Empty)
            {
                sRet = Utils.GetTcmConfigValue(sKey).Replace("Driver={SQL Server};", "");
                if (sRet.IndexOf(";Trusted_Connection=False") == -1)
                    sRet += ";Trusted_Connection=False";
            }

            // support 2012-AlwaysOn
            bool useAlwaysOn = true;

            try
            {
                if (!string.IsNullOrEmpty(Utils.GetTcmConfigValue("UseAlwaysOn")))
                    bool.TryParse(Utils.GetTcmConfigValue("UseAlwaysOn"), out useAlwaysOn);
            }
            catch (Exception) { }

            if (useAlwaysOn)
            {
                if (!sRet.EndsWith(";")) sRet += ";";
                if (!sRet.ToLower().Contains("multisubnetfailover")) sRet += "MultiSubNetFailover=Yes;";
                if (!sRet.ToLower().Contains("applicationintent")) sRet += "ApplicationIntent=" + applicationIntent + ";";

                // route ReadOnly to slaves
                if (db_Slaves != null && db_Slaves.Count > 0 && !bIsWritable)
                {
                    Random rnd = new Random();
                    bool isSlaveOK = false;

                    while (!isSlaveOK)
                    {
                        int rndIndex = rnd.Next(0, db_Slaves.Count);
                        string slaveIP = db_Slaves[rndIndex];

                        sRet = Regex.Replace(sRet, "AmazonSQL", slaveIP, RegexOptions.IgnoreCase);

                        // check slave connection
                        SqlConnection con = new SqlConnection(sRet);
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                try
                                {
                                    con.Open();
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.CommandText = "SP_Reset_Connection";
                                    command.Connection = con;

                                    SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = bIsWritable.ToString() })
                                    {
                                        int res = command.ExecuteNonQuery();
                                    }

                                    isSlaveOK = true;
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Error while opening connection to DB", ex);

                                    // clear current connection pool
                                    System.Data.SqlClient.SqlConnection.ClearPool(con);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (sRet.ToLower().Contains("multisubnetfailover")) sRet = Regex.Replace(sRet, "MultiSubNetFailover=Yes", string.Empty, RegexOptions.IgnoreCase);
                if (sRet.ToLower().Contains("applicationintent")) sRet = Regex.Replace(sRet, "ApplicationIntent=ReadWrite", string.Empty, RegexOptions.IgnoreCase);
                sRet = sRet.TrimEnd(';');
                sRet += ';';
            }

            return sRet;
        }


        static public string GetConnectionString(string sKey, bool bIsWritable)
        {
            if (string.IsNullOrEmpty(sKey))
                sKey = "CONNECTION_STRING";
            return GetConnectionStringByKey(sKey, bIsWritable);
        }


        static private bool StartConnectionStr()
        {
            string s = GetConnectionString("", false);
            if (s != "")
            {
                m_sConnectionStr = s;
                return true;
            }
            if (Utils.GetTcmConfigValue("MSSQL_SERVER_NAME") != string.Empty)
            {
                m_sConnectionStr = "Driver={SQL Server};Server=";
                m_sConnectionStr += Utils.GetTcmConfigValue("MSSQL_SERVER_NAME");
                m_sConnectionStr += ";Database=";
                m_sConnectionStr += Utils.GetTcmConfigValue("DB_NAME");
                m_sConnectionStr += ";Uid=";
                m_sConnectionStr += Utils.GetTcmConfigValue("UN");
                m_sConnectionStr += ";Pwd=";
                m_sConnectionStr += Utils.GetTcmConfigValue("PS");
                m_sConnectionStr += ";";
                return true;
            }
            //Access mdb files
            if (HttpContext.Current.Application["DB_NAME"] != null && HttpContext.Current.Application["MSSQL_SERVER_NAME"] == null)
            {
                string sMapPath = "";
                string sLocalPath = HttpContext.Current.Server.MapPath(sMapPath);
                sLocalPath += "\\db\\" + HttpContext.Current.Application["DB_NAME"].ToString();
                m_sConnectionStr = "Driver={Microsoft Access Driver (*.mdb)};Dbq=";
                m_sConnectionStr += sLocalPath;
                m_sConnectionStr += ";Uid=;Pwd=;";
                return true;
            }
            else if (HttpContext.Current.Session != null && HttpContext.Current.Session["DB_NAME"] != null && HttpContext.Current.Session["MSSQL_SERVER_NAME"] == null)
            {
                string sMapPath = "";
                string sLocalPath = HttpContext.Current.Server.MapPath(sMapPath);
                sLocalPath += "\\db\\" + HttpContext.Current.Session["DB_NAME"].ToString();
                m_sConnectionStr = "Driver={Microsoft Access Driver (*.mdb)};Dbq=";
                m_sConnectionStr += sLocalPath;
                m_sConnectionStr += ";Uid=;Pwd=;";
                return true;
            }
            //sql server files

            if (HttpContext.Current.Application["MSSQL_SERVER_NAME"] != null)
            {
                m_sConnectionStr = "Driver={SQL Server};Server=";
                m_sConnectionStr += HttpContext.Current.Application["MSSQL_SERVER_NAME"].ToString();
                m_sConnectionStr += ";Database=";
                m_sConnectionStr += HttpContext.Current.Application["DB_NAME"].ToString();
                m_sConnectionStr += ";Uid=";
                m_sConnectionStr += HttpContext.Current.Application["UN"].ToString();
                m_sConnectionStr += ";Pwd=";
                m_sConnectionStr += HttpContext.Current.Application["PS"].ToString();
                m_sConnectionStr += ";";
                return true;
            }
            else if (HttpContext.Current.Session != null && HttpContext.Current.Session["MSSQL_SERVER_NAME"] != null)
            {
                m_sConnectionStr = "Driver={SQL Server};Server=";
                m_sConnectionStr += HttpContext.Current.Session["MSSQL_SERVER_NAME"].ToString();
                m_sConnectionStr += ";Database=";
                m_sConnectionStr += HttpContext.Current.Session["DB_NAME"].ToString();
                m_sConnectionStr += ";Uid=";
                m_sConnectionStr += HttpContext.Current.Session["UN"].ToString();
                m_sConnectionStr += ";Pwd=";
                m_sConnectionStr += HttpContext.Current.Session["PS"].ToString();
                m_sConnectionStr += ";";
                return true;
            }

            //odbc
            if (HttpContext.Current.Application["DSN"] != null)
            {
                m_sConnectionStr = "DSN=";
                m_sConnectionStr += HttpContext.Current.Application["DSN"].ToString();
                m_sConnectionStr += ";Uid=";
                m_sConnectionStr += HttpContext.Current.Application["UN"].ToString();
                m_sConnectionStr += ";Pwd=";
                m_sConnectionStr += HttpContext.Current.Application["PS"].ToString();
                m_sConnectionStr += ";";
                return true;
            }
            else if (HttpContext.Current.Session != null && HttpContext.Current.Session["DSN"] != null)
            {
                m_sConnectionStr = "DSN=";
                m_sConnectionStr += HttpContext.Current.Session["DSN"].ToString();
                m_sConnectionStr += ";Uid=";
                m_sConnectionStr += HttpContext.Current.Session["UN"].ToString();
                m_sConnectionStr += ";Pwd=";
                m_sConnectionStr += HttpContext.Current.Session["PS"].ToString();
                m_sConnectionStr += ";";
                return true;
            }
            return true;
        }

        public void Finish()
        {
            //lock(m_sConnectionStr)
            //{
            try
            {
                if (m_conn != null)
                {
                    if (m_conn.State != ConnectionState.Closed)
                    {
                        m_conn.Close();
                        m_conn.Dispose();
                    }
                    m_conn = null;
                }
            }
            catch (Exception ex)
            {
                string sMes = "While closing connection Exception occurred: " + ex.Message;
                log.Error(sMes, ex);
            }
            //}
        }

        public void GetConnection(ref SqlConnection conn)
        {
            lock (m_sConnectionStr)
            {
                if (m_conn != null && m_conn.State == ConnectionState.Open)
                {
                    conn = m_conn;
                }
                else
                {
                    if (m_sConnectionStr == "")
                    {
                        StartConnectionStr();
                    }
                    try
                    {
                        if (m_conn == null)
                            m_conn = new Connection().OpenConnection(m_sConnectionStr);
                    }
                    catch (Exception ex)
                    {
                        string sMes = "While opening connection Exception occurred: " + ex.Message;
                        log.Error(sMes, ex);
                        return;
                    }
                    conn = m_conn;
                }
            }
        }

        public void GetConnection(ref SqlCommand conn)
        {
            lock (m_sConnectionStr)
            {
                if (m_conn != null && m_conn.State == ConnectionState.Open)
                {
                    conn.Connection = m_conn;
                }
                else
                {
                    if (m_sConnectionStr == "")
                    {
                        StartConnectionStr();
                    }
                    try
                    {
                        if (m_conn == null)
                        {
                            m_conn = new Connection().OpenConnection(m_sConnectionStr);
                        }
                    }
                    catch (Exception ex)
                    {
                        string sMes = "While opening connection Exception occurred (" + m_sConnectionStr + "): " + ex.Message;
                        log.Error(sMes, ex);
                        return;
                    }
                    conn.Connection = m_conn;
                }
            }
        }

        public SqlConnection OpenConnection(string sConnectionString)
        {
            SqlConnection con = new SqlConnection(sConnectionString);
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                using (SqlCommand command = new SqlCommand())
                {
                    try
                    {
                        con.Open();
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "SP_Reset_Connection";
                        command.Connection = con;

                        SqlQueryInfo queryInfo = Utils.GetSqlDataMonitor(command);
                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = m_bIsWritable.ToString() })
                        {
                            int res = command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error while opening connection to DB", ex);

                        // clear current connection pool
                        System.Data.SqlClient.SqlConnection.ClearPool(con);
                    }
                }
            }

            return con;
        }
    }
}
