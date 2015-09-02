using CachingProvider;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CachingHelpers
{
    public class GeoBlockRules : BaseCacheHelper<List<int>>
    {
        #region Consts

        /// <summary>
        /// 24 hours
        /// </summary>
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 1440d;
        private static readonly string DEFAULT_CACHE_NAME = "GroupsCache";

        #endregion

        #region Statis members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();

        #endregion

        #region Private Members

        private ICachingService rulesCache = null;
        private readonly double cacheTime;
        private string cacheGroupConfiguration;

        #endregion

        #region Singleton

        private static GeoBlockRules instance;

        public static GeoBlockRules Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new GeoBlockRules();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor and initialization

        private GeoBlockRules()
            : base()
        {
        }

        #endregion

        #region Override Methods

        protected override List<int> BuildValue(params object[] parameters)
        {
            int groupId = (int)parameters[0];
            int countryId = (int)parameters[1];

            return ApiDAL.GetPermittedGeoBlockRulesByCountry(groupId, countryId);
        }

        #endregion

        #region Public Methods

        public List<int> GetGeoBlockRulesByCountry(int groupId, int countryId)
        {
            List<int> rules = new List<int>();

            string cacheKey = string.Format("country_to_rules_{0}_{1}", groupId, countryId);
            string mutexName = string.Concat("Group GeoBlockRules GID_", groupId);

            rules = base.Get(cacheKey, mutexName, groupId, countryId);

            return rules;
        }

        public bool Remove(int groupId, int countryId)
        {
            bool isRemoveSucceeded = false;
            string cacheKey = string.Format("country_to_rules_{0}_{1}", groupId, countryId);
            string mutexName = string.Concat("Cache Delete GeoBlockRules_", groupId);

            isRemoveSucceeded = base.Remove(cacheKey, mutexName);

            return isRemoveSucceeded;
        }

        #endregion
    }
}
