using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ODBCWrapper
{
    public class QueryRoutingDefinitions
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static QueryRoutingDefinitions instance = null;

        private static readonly string STORED_PROCEDURE_NAME = "usp_get_db_AdHocQueries_routing ";

        #region Members

        public Dictionary<string, bool> definitions;

        #endregion

        #region Instance

        public static QueryRoutingDefinitions Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new QueryRoutingDefinitions();
                        }
                    }
                }

                return instance;
            }
        }

        public QueryRoutingDefinitions()
        {
            definitions = new Dictionary<string, bool>();

            SqlCommand command = new SqlCommand();

            bool success = LayeredCache.Instance.Get<Dictionary<string, bool>>(LayeredCacheKeys.GetDbQueryRoutingKey(),
                ref definitions, GetDefinitions, new Dictionary<string, object>(), 0, LayeredCacheConfigNames.QUERIES_ROUTING_CONFIG_NAME, new List<string>()
                { LayeredCacheKeys.GetQueriesRoutingInvalidationKey() });
        }

        #endregion

        private static Tuple<Dictionary<string, bool>, bool> GetDefinitions(Dictionary<string, object> funcParams)
        {
            Tuple<Dictionary<string, bool>, bool> result = null;
            Dictionary<string, bool> definitions = new Dictionary<string, bool>();

            StoredProcedure storedProcedure = new StoredProcedure(STORED_PROCEDURE_NAME, true);
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataTable table = storedProcedure.Execute();

            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    // remove all spaces and lowercase the query, just in case
                    string query = Utils.ExtractString(row, "query_str").Replace(" ", string.Empty).ToLower();

                    // check unique-ness! don't fall for this
                    if (!definitions.ContainsKey(query))
                    {
                        definitions.Add(query, true);
                    }
                }
            }

            result = new Tuple<Dictionary<string, bool>, bool>(definitions, true);

            return result;
        }

        public bool ShouldQueryRouteToSlave(string query)
        {
            bool result = false;

            string queryKey = query.Replace(" ", string.Empty).ToLower();

            if (definitions != null && definitions.ContainsKey(queryKey))
            {
                result = definitions[queryKey];
            }

            return result;
        }
    }
}
