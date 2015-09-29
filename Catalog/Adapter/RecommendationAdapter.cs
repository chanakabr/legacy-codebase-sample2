using ApiObjects;
using KLogMonitor;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Catalog
{
    public class RecommendationAdapter
    {

        #region Static Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Locker for the entire class
        /// </summary>
        private static readonly object generalLocker = new object();

        #endregion

        #region Singleton

        private static RecommendationAdapter instance;
        /// <summary>
        /// Gets the singleton instance of the adapter controller which is relevant for the given payment gateway ID
        /// </summary>
        /// <param name="paymentGatewayId"></param>
        /// <returns></returns>
        public static RecommendationAdapter GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new RecommendationAdapter();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Private Data Members

        private CouchbaseSynchronizer configurationSynchronizer;

        #endregion

        #region Ctor

        private RecommendationAdapter()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        #endregion

        #region Public Methods

        public List<long> GetChannelRecommendations(ExternalChannel externalChannel)
        {
            List<long> assetIds = new List<long>();

            return assetIds;
        }

        #endregion

        #region Private Methods

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
