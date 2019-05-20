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
    public class GeoBlockRulesCache : BaseCacheHelper<List<int>>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static GeoBlockRulesCache instance;

        public static GeoBlockRulesCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new GeoBlockRulesCache();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor and initialization

        private GeoBlockRulesCache()
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

            string cacheKey = string.Format("{0}_country_to_rules_{1}_{2}", version, groupId, countryId);
            rules = base.Get(cacheKey, groupId, countryId);

            return rules;
        }

        public bool Remove(int groupId, int countryId)
        {
            bool isRemoveSucceeded = false;
            string cacheKey = string.Format("{0}_country_to_rules_{1}_{2}", version, groupId, countryId);
            isRemoveSucceeded = base.Remove(cacheKey);

            return isRemoveSucceeded;
        }

        #endregion
    }
}
