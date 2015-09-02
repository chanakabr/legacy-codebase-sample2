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
