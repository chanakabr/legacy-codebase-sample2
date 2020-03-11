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
    public class QueryCacheDefinitions
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static QueryCacheDefinitions instance = null;

        private static readonly string STORED_PROCEDURE_NAME = "usp_get_db_query_cached";

        #region Members

        private Dictionary<string, int> definitions;

        #endregion

        #region Instance

        public static QueryCacheDefinitions Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new QueryCacheDefinitions();
                        }
                    }
                }

                return instance;
            }
        }

        public QueryCacheDefinitions()
        {
            definitions = new Dictionary<string, int>();

            SqlCommand command = new SqlCommand();

            bool success = LayeredCache.Instance.Get<Dictionary<string, int>>(LayeredCacheKeys.GetQueryCacheDefinitionsKey(),
                ref definitions, GetDefinitions, new Dictionary<string, object>(), 0, LayeredCacheConfigNames.QUERY_CACHE_CONFIG_NAME, new List<string>()
                { LayeredCacheKeys.GetQueryCacheInvalidationKey() });
        }

        #endregion

        private static Tuple<Dictionary<string, int>, bool> GetDefinitions(Dictionary<string, object> funcParams)
        {
            Tuple<Dictionary<string, int>, bool> result = null;
            Dictionary<string, int> definitions = new Dictionary<string, int>();

            StoredProcedure storedProcedure = new StoredProcedure(STORED_PROCEDURE_NAME, true);
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataTable table = storedProcedure.Execute();

            if (table != null && table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    int time = Utils.ExtractInteger(row, "cached_in_sec");
                    // remove all spaces and lowercase the query, just in case
                    string query = Utils.ExtractString(row, "query_str").Replace(" ", string.Empty).ToLower();

                    // check unique-ness! don't fall for this
                    if (!definitions.ContainsKey(query))
                    {
                        definitions.Add(query, time);
                    }
                }
            }

            result = new Tuple<Dictionary<string, int>, bool>(definitions, true);

            return result;
        }

        public int GetCacheTime(string query)
        {
            int result = 0;

            string queryKey = query.Replace(" ", string.Empty).ToLower();

            if (definitions != null && definitions.ContainsKey(queryKey))
            {
                result = definitions[queryKey];
            }

            return result;
        }
    }
}
