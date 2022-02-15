using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Configuration;
using System.Text.RegularExpressions;
using Phx.Lib.Log;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Phx.Lib.Appconfig;
using CachingProvider.LayeredCache;

namespace ODBCWrapper
{
    public class Connection
    {
        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly List<string> _DBSlaves = (!string.IsNullOrEmpty(Phx.Lib.Appconfig.TCMClient.Settings.Instance.GetValue<string>("DB_Slaves_IPs"))) ? Phx.Lib.Appconfig.TCMClient.Settings.Instance.GetValue<string>("DB_Slaves_List").Split(':').ToList<string>() : null;

        private const string DB_NAME_CONNECTION_STRING_TEMPLATE = "{dbname}";

        public static string GetConnectionString(string dbName, string sKey, bool bIsWritable, IRoutable executer)
        {
            // get connString 
            string connString = GetConnectionString(sKey, bIsWritable, executer);

            if (connString.ToLower().Contains(DB_NAME_CONNECTION_STRING_TEMPLATE) && !string.IsNullOrEmpty(dbName))
                connString = Regex.Replace(connString, DB_NAME_CONNECTION_STRING_TEMPLATE, dbName, RegexOptions.IgnoreCase);

            //_Log.Debug($"Connecting to DB using: [{connString}]");
            return connString;
        }

        public static string GetConnectionString(string sKey, bool bIsWritable, IRoutable executer)
        {
            if (string.IsNullOrEmpty(sKey))
                sKey = "CONNECTION_STRING";
            return GetConnectionStringByKey(sKey, bIsWritable, executer);
        }

        public static string GetConnectionStringByKey(string sKey, bool shouldRouteToPrimary, IRoutable executer)
        {
            string returnValue = "";

            if (ApplicationConfiguration.Current.SqlTrafficConfiguration.ShouldUseTrafficHandler.Value)
            {
                shouldRouteToPrimary = !ShouldRouteToSecondaryByHttpContext();

                if (shouldRouteToPrimary && executer != null)
                {
                    shouldRouteToPrimary = executer.ShouldRouteToPrimary();

                    if (!shouldRouteToPrimary)
                    {
                        _Log.Debug($"Sql Traffic Handler: Specific procedure/query {executer.GetName()} is now routed to secondary, despite context routing to primary.");
                    }
                }

                string primaryOrSecondary = shouldRouteToPrimary ? "primary" : "secondary";

                if (executer == null)
                {
                    _Log.Debug($"Sql Traffic Handler: next procedure/query is routed to {primaryOrSecondary}");
                }
                else
                {
                    _Log.Debug($"Sql Traffic Handler: {executer.GetName()} is routed to {primaryOrSecondary}");
                }
            }

            string applicationIntent = (shouldRouteToPrimary) ? "ReadWrite" : "ReadOnly";

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
                useAlwaysOn = ApplicationConfiguration.Current.DatabaseConfiguration.UseAlwaysOn.Value;
            }
            catch (Exception) { }

            if (useAlwaysOn)
            {
                if (!returnValue.EndsWith(";")) returnValue += ";";
                if (!returnValue.ToLower().Contains("multisubnetfailover")) returnValue += "MultiSubNetFailover=Yes;";
                if (!returnValue.ToLower().Contains("applicationintent")) returnValue += "ApplicationIntent=" + applicationIntent + ";";

                // route ReadOnly to slaves
                if (_DBSlaves != null && _DBSlaves.Count > 0 && !shouldRouteToPrimary)
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
                                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = queryInfo.Database, QueryType = queryInfo.QueryType, Table = queryInfo.Table, IsWritable = shouldRouteToPrimary.ToString() })
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

        protected internal static bool ShouldRouteToSecondaryByHttpContext()
        {
            return HttpContext.Current != null && HttpContext.Current.Items != null &&
                // either we have general key that routes *all* requests to secondary
                ((HttpContext.Current.Items[LayeredCache.CONTEXT_KEY_SHOULD_ROUTE_DB_TO_SECONDARY] != null && Convert.ToBoolean(HttpContext.Current.Items[LayeredCache.CONTEXT_KEY_SHOULD_ROUTE_DB_TO_SECONDARY])) ||
                // or only thread-specific key routes *next* request to secondary
                (HttpContext.Current.Items[LayeredCache.GetCurrentThreadDbRoutingContextKey()] != null && Convert.ToBoolean(HttpContext.Current.Items[LayeredCache.GetCurrentThreadDbRoutingContextKey()])));
        }
    }
}
