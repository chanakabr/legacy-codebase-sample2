using ApiObjects;
using ApiObjects.Response;
using CachingHelpers;
using KLogMonitor;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using System.ServiceModel;

namespace AdapterControllers
{
    public class RecommendationAdapterController
    {
        #region Consts

        private const int STATUS_OK = 0;
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;

        private const string PARAMETER_ENGINE = "engine";
        private const string PARAMETER_GROUP_ID = "group_id";

        #endregion

        #region Static Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Locker for the entire class
        /// </summary>
        private static readonly object generalLocker = new object();

        #endregion

        #region Singleton

        private static RecommendationAdapterController instance;

        /// <summary>
        /// Gets the singleton instance of the adapter controller
        /// </summary>
        /// <param name="paymentGatewayId"></param>
        /// <returns></returns>
        public static RecommendationAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new RecommendationAdapterController();
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

        private RecommendationAdapterController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        #endregion

        #region Public Methods
        
        public List<RecommendationResult> GetChannelRecommendations(ExternalChannel externalChannel, 
            Dictionary<string, string> enrichments, string free, out string requestId, int pageIndex, int pageSize, out int totalResults)
        {
            totalResults = 0;
            List<RecommendationResult> searchResults = new List<RecommendationResult>();

            RecommendationEngine engine = RecommendationEnginesCache.Instance().GetRecommendationEngine(externalChannel.GroupId, externalChannel.RecommendationEngineId);

            if (engine == null)
            {
                throw new KalturaException(string.Format("Recommendation Engine {0} doesn't exist", externalChannel.RecommendationEngineId), (int)eResponseStatus.RecommendationEngineNotExist);
            }
            
            if (string.IsNullOrEmpty(engine.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }
            
            RecommendationEngineAdapter.ServiceClient adapterClient = new RecommendationEngineAdapter.ServiceClient(string.Empty, engine.AdapterUrl);

            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress("http://localhost/REAdapter/service.svc");//engine.AdapterUrl);
            
            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature =
                string.Concat(externalChannel.ID, engine.ID, unixTimestamp);

            var enrichmentsList =
                enrichments.Select(item => new RecommendationEngineAdapter.KeyValue()
                {
                    Key = item.Key,
                    Value = item.Value
                });

            try
            {
                string enrichmentsString = string.Join(";", enrichmentsList.Select(item => string.Concat("Key: ", item.Key, ", Value: ", item.Value)));

                log.DebugFormat("Sending request to recommendation engine adapter. Channel ID = {0}, engine = {1}, enrichments = {2}, pageIndex = {3}, pageSize = {4}",
                    externalChannel.ID,
                    engine.ID,
                    enrichmentsString,
                    pageIndex,
                    pageSize);

                var adapterResponse = new RecommendationEngineAdapter.RecommendationsResult();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = adapterClient.GetChannelRecommendations(engine.ID,
                        externalChannel.ExternalIdentifier,
                        enrichmentsList.ToArray(),
                        free,
                        unixTimestamp,
                        System.Convert.ToBase64String(
                            EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))),
                        pageIndex,
                        pageSize);
                }

                requestId = adapterResponse.RequestId;

                LogAdapterResponse(adapterResponse, "GetChannelRecommendation");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("RecommendationsEngine_Adapter_Locker_{0}", engine.ID);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ENGINE, engine},
                        {PARAMETER_GROUP_ID, externalChannel.GroupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = adapterClient.GetChannelRecommendations(engine.ID,
                            externalChannel.ExternalIdentifier,
                            enrichmentsList.ToArray(),
                            free,
                            unixTimestamp,
                            System.Convert.ToBase64String(
                                EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))),
                            pageIndex,
                            pageSize);
                    }

                    requestId = adapterResponse.RequestId;

                    LogAdapterResponse(adapterResponse, "GetChannelRecommendation"); 

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Results != null)
                    {
                        searchResults =
                            adapterResponse.Results.Select(result =>
                                new RecommendationResult()
                                {
                                    id = result.AssetId,
                                    type = (eAssetTypes)result.AssetType
                                }).ToList();
                    }
                    totalResults = adapterResponse.TotalResults;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in get channel recommendations: error = {0} ",
                    ex,
                    engine.ID,
                    externalChannel.ID
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return searchResults;
        }

        public List<RecommendationResult> GetRelatedRecommendations(int recommendationEngineId, Int32 nMediaID, Int32 nMediaTypeID, Int32 nGroupID, string siteGuid, string deviceId, 
                                                                    string language, int utcOffset, string sUserIP, string sSignature, string sSignString, List<Int32> filterTypeIDs, Int32 nPageSize,
                                                                    Int32 nPageIndex, Dictionary<string, string> enrichments, string freeParam, out string requestId)
        {
            List<RecommendationResult> searchResults = new List<RecommendationResult>();

            RecommendationEngine engine = RecommendationEnginesCache.Instance().GetRecommendationEngine(nGroupID, recommendationEngineId);

            if (engine == null)
            {
                throw new KalturaException(string.Format("Recommendation Engine {0} doesn't exist", recommendationEngineId), (int)eResponseStatus.RecommendationEngineNotExist);
            }
            
            if (string.IsNullOrEmpty(engine.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            RecommendationEngineAdapter.ServiceClient adapterClient = new RecommendationEngineAdapter.ServiceClient(string.Empty, engine.AdapterUrl);

            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(engine.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature =
                string.Concat(sSignature, unixTimestamp);

            var enrichmentsList =
                enrichments.Select(item => new RecommendationEngineAdapter.KeyValue()
                {
                    Key = item.Key,
                    Value = item.Value
                });

            try
            {
                string enrichmentsString = string.Join(";", enrichmentsList.Select(item => string.Concat("Key: ", item.Key, ", Value: ", item.Value)));

                log.DebugFormat("Sending related request to recommendation engine adapter. media ID = {0}, engine = {1}, user ID = {2}, device ID = {3}, language = {4}, utcOffset = {5}, page index = {6}, page size = {7}, enrichments = {8}",
                    nMediaID, engine.ID, siteGuid, deviceId, language, utcOffset, nPageIndex, nPageSize, enrichmentsString);

                var adapterResponse = new RecommendationEngineAdapter.RecommendationsResult();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = adapterClient.GetRelatedRecommendations(engine.ID,
                        nMediaID, nMediaTypeID,
                        enrichmentsList.ToArray(), freeParam, filterTypeIDs.ToArray(), nPageIndex, nPageSize,
                        unixTimestamp,
                        System.Convert.ToBase64String(
                            EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))));
                }

                requestId = adapterResponse.RequestId;

                LogAdapterResponse(adapterResponse, "GetRelatedRecommendation");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("RecommendationsEngine_Adapter_Locker_{0}", engine.ID);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ENGINE, engine},
                        {PARAMETER_GROUP_ID, nGroupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get related recommendations - after it is configured
                        adapterResponse = adapterClient.GetRelatedRecommendations(engine.ID, nMediaID, nMediaTypeID, 
                            enrichmentsList.ToArray(), freeParam, filterTypeIDs.ToArray(),nPageIndex, nPageSize, unixTimestamp,
                            System.Convert.ToBase64String(
                                EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))));
                    }

                    requestId = adapterResponse.RequestId;

                    LogAdapterResponse(adapterResponse, "GetRelatedRecommendation");

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Results != null)
                    {
                        searchResults =
                            adapterResponse.Results.Select(result =>
                                new RecommendationResult()
                                {
                                    id = result.AssetId,
                                    type = (eAssetTypes)result.AssetType
                                }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in get related recommendations: error = {0} ", ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return searchResults;
        }

        public List<RecommendationResult> GetSearchRecommendations(int recommendationEngineId, string query, Int32 nGroupID, string siteGuid, string deviceId,
                                                                    string language, int utcOffset, string sUserIP, string sSignature, string sSignString, List<Int32> filterTypeIDs, Int32 nPageSize,
                                                                    Int32 nPageIndex, Dictionary<string, string> enrichments, out string requestId)
        {
            List<RecommendationResult> searchResults = new List<RecommendationResult>();

            RecommendationEngine engine = RecommendationEnginesCache.Instance().GetRecommendationEngine(nGroupID, recommendationEngineId);
            
            if (engine == null)
            {
                throw new KalturaException(string.Format("Recommendation Engine {0} doesn't exist", recommendationEngineId), (int)eResponseStatus.RecommendationEngineNotExist);
            }
            
            if (string.IsNullOrEmpty(engine.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            RecommendationEngineAdapter.ServiceClient adapterClient = new RecommendationEngineAdapter.ServiceClient(string.Empty, engine.AdapterUrl);

            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(engine.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature =
                string.Concat(sSignature, unixTimestamp);

            var enrichmentsList =
                enrichments.Select(item => new RecommendationEngineAdapter.KeyValue()
                {
                    Key = item.Key,
                    Value = item.Value
                });

            try
            {
                string enrichmentsString = string.Join(";", enrichmentsList.Select(item => string.Concat("Key: ", item.Key, ", Value: ", item.Value)));

                log.DebugFormat("Sending search request to recommendation engine adapter. query = {0}, engine = {1}, user ID = {2}, device ID = {3}, language = {4}, utcOffset = {5}, page index = {6}, page size = {7}, enrichments = {8}",
                    query, engine.ID, siteGuid, deviceId, language, utcOffset, nPageIndex, nPageSize, enrichmentsString);

                var adapterResponse = new RecommendationEngineAdapter.RecommendationsResult();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = adapterClient.GetSearchRecommendations(engine.ID,
                        query,
                        enrichmentsList.ToArray(), filterTypeIDs.ToArray(), nPageIndex, nPageSize,
                        unixTimestamp,
                        System.Convert.ToBase64String(
                            EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))));
                }

                requestId = adapterResponse.RequestId;

                LogAdapterResponse(adapterResponse, "GetSearchRecommendation");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("RecommendationsEngine_Adapter_Locker_{0}", engine.ID);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ENGINE, engine},
                        {PARAMETER_GROUP_ID, nGroupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get Search recommendations - after it is configured
                        adapterResponse = adapterClient.GetSearchRecommendations(engine.ID,
                            query,
                            enrichmentsList.ToArray(), filterTypeIDs.ToArray(), nPageIndex, nPageSize,
                            unixTimestamp,
                            System.Convert.ToBase64String(
                                EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))));
                    }
                    requestId = adapterResponse.RequestId;

                    LogAdapterResponse(adapterResponse, "GetSearchRecommendation");

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Results != null)
                    {
                        searchResults =
                            adapterResponse.Results.Select(result =>
                                new RecommendationResult()
                                {
                                    id = result.AssetId,
                                    type = (eAssetTypes)result.AssetType
                                }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in get Search recommendations: error = {0} ", ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return searchResults;
        }

        private void LogAdapterResponse(RecommendationEngineAdapter.RecommendationsResult adapterResponse, string action)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("Recommendation Engine Adapter {0} Result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("Recommendation Engine Adapter {0} Result's status is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Results == null)
            {
                logMessage = string.Format("Recommendation Engine Adapter {0} Result Status: Message = {1}, Code = {2}",
                                 action != null ? action : string.Empty,                                                                                                                // {0}
                                 adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                 adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
            }
            else
            {
                string recommendationsString = 
                    string.Join(";", adapterResponse.Results.Select(item => string.Concat("ID: ", item.AssetId, ", Type: ", item.AssetType.ToString())));

                logMessage = string.Format("Recommendation Engine Adapter {0} Result Status: Message = {1}, " +
                    "Code = {2} Count = {3}, List = {4}",
                    action != null ? action : string.Empty,  // {0}
                    adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,  // {1}
                    adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1, // {2}
                    adapterResponse != null && adapterResponse.Results != null ? adapterResponse.Results.Length : -1,
                    recommendationsString); // {3}
            }

            log.Debug(logMessage);
        }

        public void ShareFilteredResponse(ExternalChannel externalChannel, List<RecommendationResult> results)
        {
            RecommendationEngine engine =
                RecommendationEnginesCache.Instance().GetRecommendationEngine(externalChannel.GroupId, externalChannel.RecommendationEngineId);

            RecommendationEngineAdapter.ServiceClient adapterClient = new RecommendationEngineAdapter.ServiceClient(string.Empty, engine.AdapterUrl);

            if (!string.IsNullOrEmpty(engine.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(engine.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature =
                string.Concat(externalChannel.ID, engine.ID, unixTimestamp);

            RecommendationEngineAdapter.SearchResult[] resultsArray = results.Select(item =>
                new RecommendationEngineAdapter.SearchResult()
                {
                    AssetId = item.id,
                    AssetType = ConvertAssetType(item.type)
                }).ToArray();

            // Call share filtered response - asynchronously
            Task task = Task.Factory.StartNew(() => adapterClient.ShareFilteredResponse(engine.ID, resultsArray));
        }

        #endregion

        #region Private Methods

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                RecommendationEngine engine = null;
                int groupId = 0;

                if (parameters.ContainsKey(PARAMETER_ENGINE))
                {
                    engine = (RecommendationEngine)parameters[PARAMETER_ENGINE];
                }

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                result = this.SendConfiguration(engine, groupId);
            }

            return result;
        }

        public bool SendConfiguration(RecommendationEngine engine, int groupId)
        {
            bool result = false;

            if (engine != null && !string.IsNullOrEmpty(engine.AdapterUrl))
            {
                RecommendationEngineAdapter.ServiceClient client = new RecommendationEngineAdapter.ServiceClient(string.Empty, engine.AdapterUrl);

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Empty;

                try
                {
                    //call Adapter Transact
                    RecommendationEngineAdapter.AdapterStatus adapterResponse =
                        client.SetConfiguration(engine.ID,
                        engine.Settings != null ? engine.Settings.Select(setting => new RecommendationEngineAdapter.KeyValue()
                        {
                            Key = setting.key,
                            Value = setting.value
                        }).ToArray() : null,
                        groupId,
                        unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(engine.SharedSecret, EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null)
                        log.DebugFormat("Recommendations Engine Adapter Send Configuration Result = {0}", adapterResponse);
                    else
                        log.Debug("Adapter response is null");

                    if (adapterResponse != null && adapterResponse.Code == STATUS_OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, engine id = {1}", ex, engine != null ? engine.ID : 0);
                }
            }

            return result;
        }

        private RecommendationEngineAdapter.eAssetTypes ConvertAssetType(eAssetTypes origin)
        {
            switch (origin)
            {
                case eAssetTypes.UNKNOWN:
                return RecommendationEngineAdapter.eAssetTypes.UNKNOWN;
                case eAssetTypes.EPG:
                return RecommendationEngineAdapter.eAssetTypes.EPG;
                case eAssetTypes.NPVR:
                return RecommendationEngineAdapter.eAssetTypes.NPVR;
                case eAssetTypes.MEDIA:
                return RecommendationEngineAdapter.eAssetTypes.MEDIA;
                default:
                return RecommendationEngineAdapter.eAssetTypes.UNKNOWN;
            }
        }

        #endregion
    }
}
