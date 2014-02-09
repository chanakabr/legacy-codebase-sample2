using System;
using System.Data;
using System.Data.Odbc;
using System.Web;
using System.Configuration;

namespace ODBCWrapper
{
	public class Connection 
	{
        public delegate string GetConnectionStringDelegate();

        #region Fields        
        public string CustomConnectionString { get; set; }
        public static GetConnectionStringDelegate GetDefaultConnectionStringMethod { get; set; }
        public GetConnectionStringDelegate GetConnectionStringMethod { get; set; }

        private OdbcConnection m_conn = null;
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
            else  if (GetDefaultConnectionStringMethod != null)
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

        public Connection() : this(null)
        {
            
        }

        public Connection(GetConnectionStringDelegate getConnectionStringMethod)
        {
            GetConnectionStringMethod = getConnectionStringMethod;
        }
        
        private static string defaultConnectionString()
        {
            return string.Concat("Driver={SQL Server};Server=",HttpContext.Current.Application["MSSQL_SERVER_NAME"].ToString(),
            ";Database=",HttpContext.Current.Application["DB_NAME"].ToString(),
            ";Uid=",HttpContext.Current.Application["UN"].ToString(),
            ";Pwd=",HttpContext.Current.Application["PS"].ToString(),
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
                string sMes = "While closing connection Exception occured: " + ex.Message;
                Logger.Logger.Log("connection", sMes, "ODBC_Net");
                Logger.Logger.Log("connection", sMes, "ODBC_Connections");
            }
        }

        public OdbcConnection GetConnection()
        {
            if (m_conn == null)
            {
                string connectionString = getConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Member 'GetConnectionStringMethod' returned with empty string as connection string");
                }

                m_conn = new OdbcConnection(connectionString);
            }

            if (m_conn.State != ConnectionState.Open)
            {
                m_conn.Open();
            }

             return m_conn;
        }

	    public void GetConnection(ref OdbcConnection conn)
	    {
            conn = GetConnection();	        
	    }

        public void GetConnection(ref OdbcCommand conn)
        {
            conn.Connection = GetConnection();
        }
	    #endregion
	}
}
