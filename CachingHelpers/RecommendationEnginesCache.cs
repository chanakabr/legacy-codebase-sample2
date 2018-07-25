using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Core.DAL;

namespace CachingHelpers
{
    public class RecommendationEnginesCache : BaseCacheHelper<RecommendationEngine>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static RecommendationEnginesCache instance;

        public static RecommendationEnginesCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new RecommendationEnginesCache();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private RecommendationEnginesCache()
            : base()
        {

        }

        #endregion

        #region Override Methods

        protected override RecommendationEngine BuildValue(params object[] parameters)
        {
            int engineId = (int)parameters[0];
            int groupId = (int)parameters[1];

            return CatalogDAL.GetRecommendationEngine(groupId, engineId);
        }

        public RecommendationEngine GetRecommendationEngine(int groupId, int engineId)
        {
            string cacheKey = string.Format("{0}_recommendation_engine_{1}", version, engineId);
            return base.Get(cacheKey, engineId, groupId);
        }

        #endregion
    }
}
