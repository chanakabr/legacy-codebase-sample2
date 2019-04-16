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
using ConfigurationManager;

namespace ODBCWrapper
{
    public class Connection
    {
        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly List<string> _DBSlaves = (!string.IsNullOrEmpty(TCMClient.Settings.Instance.GetValue<string>("DB_Slaves_IPs"))) ? TCMClient.Settings.Instance.GetValue<string>("DB_Slaves_List").Split(':').ToList<string>() : null;

        public static bool _IsWritable;
        private const string DB_NAME_CONNECTION_STRING_TEMPLATE = "{dbname}";        
        private SqlConnection _Conn = null;
        private static string _ConnectionStr = "";

        static public string GetConnectionString(string dbName, string sKey, bool bIsWritable)
        {
            // get connString 
            string connString = GetConnectionString(sKey, bIsWritable);

            if (connString.ToLower().Contains(DB_NAME_CONNECTION_STRING_TEMPLATE) && !string.IsNullOrEmpty(dbName))
                connString = Regex.Replace(connString, DB_NAME_CONNECTION_STRING_TEMPLATE, dbName, RegexOptions.IgnoreCase);

            _Log.Debug($"Connecting to DB using: [{connString}]");
            return connString;
        }

        //TODO : add connection string for WRITABLE 
        static public string GetConnectionStringByKey(string sKey, bool bIsWritable)
        {
            string returnValue = "";            
            string applicationIntent = (bIsWritable) ? "ReadWrite" : "ReadOnly";
            _IsWritable = bIsWritable;

            var tcmValue = Utils.GetTcmConfigValue(sKey);
            if (!string.IsNullOrEmpty(tcmValue))
            {
                returnValue = tcmValue.Replace("Driver={SQL Server};", "");
                if (returnValue.IndexOf(";Trusted_Connection=False") == -1)
                    returnValue += ";Trusted_Connection=False";
            }

            // support 2012-AlwaysOn
            bool useAlwaysOn = true;

            try
            {
                useAlwaysOn = ApplicationConfiguration.DatabaseConfiguration.UseAlwaysOn.Value;
            }
            catch (Exception) { }

            if (useAlwaysOn)
            {
                if (!returnValue.EndsWith(";")) returnValue += ";";
                if (!returnValue.ToLower().Contains("multisubnetfailover")) returnValue += "MultiSubNetFailover=Yes;";
                if (!returnValue.ToLower().Contains("applicationintent")) returnValue += "ApplicationIntent=" + applicationIntent + ";";

                // route ReadOnly to slaves
                if (_DBSlaves != null && _DBSlaves.Count > 0 && !bIsWritable)
                {
                    Random rnd = new Random();
                    bool isSlaveOK = false;

                    while (!isSlaveOK)
                    {
                        int rndIndex = rnd.Next(0, _DBSlaves.Count);
                        string slaveIP = _DBSlaves[rndIndex];

                        returnValue = Regex.Replace(returnValue, "AmazonSQL", slaveIP, RegexOptions.IgnoreCase);

                        // check slave connection
                        SqlConnection con = new SqlConnection(returnValue);
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
                                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (_IsWritable || Utils.UseWritable).ToString() })
                                    {
                                        int res = command.ExecuteNonQuery();
                                    }

                                    isSlaveOK = true;
                                }
                                catch (Exception ex)
                                {
                                    _Log.Error("Error while opening connection to DB", ex);

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
                if (returnValue.ToLower().Contains("multisubnetfailover")) returnValue = Regex.Replace(returnValue, "MultiSubNetFailover=Yes", string.Empty, RegexOptions.IgnoreCase);
                if (returnValue.ToLower().Contains("applicationintent")) returnValue = Regex.Replace(returnValue, "ApplicationIntent=ReadWrite", string.Empty, RegexOptions.IgnoreCase);
                returnValue = returnValue.TrimEnd(';');
                returnValue += ';';
            }

            return returnValue;
        }

        static public string GetConnectionString(string sKey, bool bIsWritable)
        {
            if (string.IsNullOrEmpty(sKey))
                sKey = "CONNECTION_STRING";
            return GetConnectionStringByKey(sKey, bIsWritable);
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
                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = (_IsWritable || Utils.UseWritable).ToString() })
                        {
                            int res = command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        _Log.Error("Error while opening connection to DB", ex);

                        // clear current connection pool
                        System.Data.SqlClient.SqlConnection.ClearPool(con);
                    }
                }
            }

            return con;
        }

    }
}
