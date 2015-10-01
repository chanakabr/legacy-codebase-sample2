using ApiObjects;
using CachingHelpers;
using Catalog.Response;
using KLogMonitor;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;

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
        /// Gets the singleton instance of the adapter controller
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
        
        public List<UnifiedSearchResult> GetChannelRecommendations(ExternalChannel externalChannel, Dictionary<string, string> enrichments)
        {
            List<UnifiedSearchResult> searchResults = new List<UnifiedSearchResult>();

            RecommendationEngine engine = RecommendationEnginesCache.Instance().GetRecommendationEngine(externalChannel.groupId, externalChannel.recommendationEngineId);

            RecommendationsEnginesAdapter.ServiceClient adapterClient = new RecommendationsEnginesAdapter.ServiceClient(string.Empty, engine.AdapterUrl);

            if (!string.IsNullOrEmpty(engine.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(engine.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Empty;
                //string.Concat(this.paymentGatewayId, request.siteGuid, request.chargeId, request.price, request.currency,
                //request.productId, request.productType, request.contentId, request.userIP, unixTimestamp);

            var enrichmentsList =
                enrichments.Select(item => new RecommendationsEnginesAdapter.KeyValue()
                {
                    Key = item.Key,
                    Value = item.Value
                });

            try
            {
                //call Adapter get channel recommendations
                var adapterResponse = adapterClient.GetChannelRecommendations(engine.ID,
                    externalChannel.externalId,
                    enrichmentsList.ToArray(),
                    unixTimestamp,
                    System.Convert.ToBase64String(
                        AdapterUtils.AesEncrypt(engine.SharedSecret, AdapterUtils.HashSHA1(signature))));

                LogAdapterResponse(adapterResponse, "GetChannelRecommendation");

                //if (adapterResponse != null && adapterResponse.Status != null &&
                //    adapterResponse.Status.Code == (int)RecommendationsEnginesAdapter.NoConfigurationFound)
                //{
                //    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                //    // Build dictionary for synchronized action
                //    Dictionary<string, object> parameters = new Dictionary<string, object>()
                //    {
                //        {PARAMETER_PAYMENT_GATEWAY, request.paymentGateway},
                //        {PARAMETER_GROUP_ID, request.groupId}
                //    };

                //    configurationSynchronizer.DoAction(key, parameters);

                //    //call Adapter Transact - after it is configured
                //    adapterResponse = adapterClient.Transact(this.paymentGatewayId,
                //        request.siteGuid, request.chargeId,
                //        request.price, request.currency, request.productId.ToString(),
                //        ConvertTransactionType(request.productType),
                //        request.contentId.ToString(), request.userIP,
                //        unixTimestamp,
                //        System.Convert.ToBase64String(
                //            Billing.Utils.AesEncrypt(request.paymentGateway.SharedSecret, Billing.Utils.HashSHA1(signature))));

                //    LogAdapterResponse(adapterResponse, "Transact");
                //}
            }
            catch (Exception ex)
            {   
            }

            //return adapterResponse;
            return searchResults;
        }

        private void LogAdapterResponse(RecommendationsEnginesAdapter.RecommendationsResult adapterResponse, string p)
        {
            throw new NotImplementedException();
        }


        public void ShareFilteredResponse(ExternalChannel externalChannel, List<UnifiedSearchResult> results)
        {

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
