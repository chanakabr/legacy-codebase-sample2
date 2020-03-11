using System;
using System.Data;
using System.Web;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ODBCWrapper
{
    public class Connection
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public delegate string GetConnectionStringDelegate();

        #region Fields
        public string CustomConnectionString { get; set; }
        public static GetConnectionStringDelegate GetDefaultConnectionStringMethod { get; set; }
        public GetConnectionStringDelegate GetConnectionStringMethod { get; set; }

        private SqlConnection m_conn = null;
        #endregion

        private string getConnectionString()
        {
            if (!string.IsNullOrEmpty(CustomConnectionString))
            {
                return CustomConnectionString;
            }

            if (GetConnectionStringMethod != null)
            {
                return GetConnectionStringMethod();
            }
            else if (GetDefaultConnectionStringMethod != null)
            {
                return GetDefaultConnectionStringMethod();
            }

            throw new Exception("One of the mothods must be assigned before use");
        }

        #region Constructor
        static Connection()
        {
            GetDefaultConnectionStringMethod = defaultConnectionString;
        }
        #endregion

        public Connection()
            : this(null)
        {

        }

        public Connection(GetConnectionStringDelegate getConnectionStringMethod)
        {
            GetConnectionStringMethod = getConnectionStringMethod;
        }

        private static string defaultConnectionString()
        {
            return string.Concat("Driver={SQL Server};Server=", HttpContext.Current.Application["MSSQL_SERVER_NAME"].ToString(),
            ";Database=", HttpContext.Current.Application["DB_NAME"].ToString(),
            ";Uid=", HttpContext.Current.Application["UN"].ToString(),
            ";Pwd=", HttpContext.Current.Application["PS"].ToString(),
            ";");
        }

        #region Public Methods
        public void Finish()
        {
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
                string message = "While closing connection Exception occurred: " + ex.Message;
                logger.Error(message, ex);
            }
        }

        public SqlConnection GetConnection()
        {
            if (this.m_conn == null)
            {
                string ConnStr = getConnectionString();
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseSQL2008"]) || !bool.Parse(ConfigurationManager.AppSettings["UseSQL2008"]))
                {
                    if (ConnStr.ToLower().Contains("driver={sql server};"))
                    {
                        ConnStr = Regex.Replace(ConnStr, "driver={sql server};", string.Empty, RegexOptions.IgnoreCase);
                    }
                    if (!ConnStr.EndsWith(";"))
                    {
                        ConnStr = ConnStr + ";";
                    }
                    if (!ConnStr.ToLower().Contains("multisubnetfailover"))
                    {
                        ConnStr = ConnStr + "MultiSubNetFailover=Yes;";
                    }
                    if (!ConnStr.ToLower().Contains("applicationintent"))
                    {
                        ConnStr = ConnStr + "ApplicationIntent=ReadWrite;";
                    }
                }
                if (string.IsNullOrEmpty(ConnStr))
                {
                    throw new Exception("Member 'GetConnectionStringMethod' returned with empty string as connection string");
                }
                this.m_conn = new SqlConnection(ConnStr);
            }
            if (this.m_conn.State != ConnectionState.Open)
            {
                this.m_conn.Open();
            }
            using (new SqlDataAdapter())
            {
                using (SqlCommand command = new SqlCommand())
                {
                    try
                    {
                        //Logger.Logger.Log("connection", this.m_conn.ConnectionString, "ODBC_Connections");
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "SP_Reset_Connection";
                        command.Connection = this.m_conn;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        //Logger.Logger.Log("Connection", exception.ToString(), "ODBC_Net");
                        SqlConnection.ClearPool(m_conn);
                    }
                }
            }
            return this.m_conn;

        }

        public void GetConnection(ref SqlConnection conn)
        {
            conn = GetConnection();
        }

        public void GetConnection(ref SqlCommand conn)
        {
            conn.Connection = GetConnection();
        }
        #endregion
    }
}
