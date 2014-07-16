using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Configuration;
using System.Text.RegularExpressions;

namespace ODBCWrapper
{
    public class Connection
    {
        #region Constructor
        public Connection()
        {
        }
        #endregion

        #region Fields
        private SqlConnection m_conn = null;

        static private string m_sConnectionStr = "";
        protected object m_crit_sec = new object();
        #endregion

        #region Static Methods
        static public void ClearConnection()
        {
            m_sConnectionStr = "";
        }

        //TODO : add connection string for WRITABLE 
        static public string GetConnectionStringByKey(string sKey, bool bIsWritable)
        {
            string sRet = "";
            
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
                if (!sRet.ToLower().Contains("applicationintent")) sRet += "ApplicationIntent=ReadWrite;";
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
        #endregion

        #region Public Methods
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
                string sMes = "While closing connection Exception accured: " + ex.Message;
                Logger.Logger.Log("connection", sMes, "ODBC_Net");
                Logger.Logger.Log("connection", sMes, "ODBC_Connections");
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
                        string sMes = "While openning connection Exception accured: " + ex.Message;
                        Logger.Logger.Log("connection", sMes, "ODBC_Net");
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
                        string sMes = "While openning connection Exception accured (" + m_sConnectionStr + "): " + ex.Message;
                        Logger.Logger.Log("Connection", sMes, "ODBC_Net");
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
                        int res = command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log("Connection", ex.ToString(), "ODBC_Net");

                        // clear current connection pool
                        System.Data.SqlClient.SqlConnection.ClearPool(con);
                    }
                }
            }

            return con;
        }
        #endregion
    }
}
